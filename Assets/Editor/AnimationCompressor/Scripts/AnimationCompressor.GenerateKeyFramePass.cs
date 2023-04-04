using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace AnimationCompressor
{
    public partial class Core
    {
        private void GenerateKeyFrameByCurveFittingPass()
        {
            var curveBindings = AnimationUtility.GetCurveBindings(originClip);

            Debug.LogError("=====" + curveBindings.Length);


            var recordNameDict = new Dictionary<string, int>();
            foreach (var curveBinding in curveBindings)
            {
                var isTansformCurve = Util.IsTransformKey(curveBinding.propertyName);
                var originCurve = AnimationUtility.GetEditorCurve(originClip, curveBinding);
                var compressCurve = AnimationUtility.GetEditorCurve(originClip, curveBinding);      // copy curve

                Debug.LogError(string.Format("---{0} {1} {2}", curveBinding.path, curveBinding.propertyName, originCurve.keys.Length));

                // Only working on transform keys
                if (isTansformCurve)
                {
                    // Clear key, gen key
                    compressCurve.keys = null;
                    GenerateKeyFrameByCurveFitting(curveBinding, originCurve, compressCurve);
                }

                compressClip.SetCurve(curveBinding.path, curveBinding.type, curveBinding.propertyName, compressCurve);


                if (!recordNameDict.ContainsKey(curveBinding.propertyName))
                {
                    recordNameDict[curveBinding.propertyName] = 0;
                }
                recordNameDict[curveBinding.propertyName]+=compressCurve.keys.Length;
            }

            foreach(var item in recordNameDict)
            {
                Debug.LogError("记录: " + item.Key + " " + item.Value);
            }
        }

        /// <summary>
        /// 키프레임 재생성
        /// </summary>
        /// <param name="originCurve"></param>
        /// <param name="compressCurve"></param>
        /// <param name="allowErrorRange"></param>
        private void GenerateKeyFrameByCurveFitting(EditorCurveBinding curveBinding, AnimationCurve originCurve, AnimationCurve compressCurve)
        {
            var propertyName = curveBinding.propertyName;
            var path = curveBinding.path;
            var depth = Util.GetDepth(path);
            var allowErrorRange = GetAllowErrorValue(propertyName, depth);

            // Add first, last key
            compressCurve.AddKey(originCurve.keys[0]);
            compressCurve.AddKey(originCurve.keys[originCurve.keys.Length - 1]);

            if (originCurve.keys.Length <= 2)
                return;


            //格拉斯-普克算法 也叫 迭代端点拟合算法
            var itrCount = 0f;
            while (true)
            {
                var tick = 0f;
                // var time = originCurve.keys[originCurve.keys.Length - 1].time;  好像没啥用

                var highestOffset = -1f;
                Keyframe highestKey = new Keyframe();

                for (var i = 0; i < originCurve.keys.Length; i++)
                {
                    tick = originCurve.keys[i].time;
                    var orgEv = originCurve.Evaluate(tick);
                    var compEv = compressCurve.Evaluate(tick);
                    var offset = Mathf.Abs(orgEv - compEv);

                    if (offset >= allowErrorRange)
                    {
                        if (offset > highestOffset)
                        {
                            highestOffset = offset;
                            highestKey = originCurve.keys[i];
                        }
                    }
                }

                if (highestOffset == -1f)
                    break;

                compressCurve.AddKey(highestKey);
                itrCount++;
            }

            Debug.Log($"{nameof(AnimationCompressor)} itrCount : {itrCount}");
        }

        private float GetAllowErrorValue(string propertyName, int depth = 1)
        {
            // Restrict divide by zero
            depth = Mathf.Max(1, depth);

            var fDepth = (float)depth;

            var revDepth = maxDepth - depth;
            var multiplier = (float)revDepth;

            switch (propertyName)
            {
                case "m_LocalPosition":
                case "m_LocalPosition.x":
                case "m_LocalPosition.y":
                case "m_LocalPosition.z":
                    return option.PositionAllowError; // fDepth;

                case "m_LocalRotation":
                case "m_LocalRotation.x":
                case "m_LocalRotation.y":
                case "m_LocalRotation.z":
                case "m_LocalRotation.w":
                    return option.RotationAllowError; // fDepth;

                case "m_LocalScale":
                case "m_LocalScale.y":
                case "m_LocalScale.z":
                case "m_LocalScale.x":
                    return option.ScaleAllowError;      /// fDepth;

                default:
                    return 0f;
            }
        }
    }
}