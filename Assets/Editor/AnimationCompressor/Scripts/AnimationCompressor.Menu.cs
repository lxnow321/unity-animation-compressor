using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AnimationCompressor
{
    public class Menu
    {
        [MenuItem("测试/压缩AnimationClip")]
        public static void CompressSelectedAnimationClips()
        {
            var clips = new List<AnimationClip>();
            var selections = Selection.objects;

            foreach (var selection in selections)
            {
                if (selection is AnimationClip == false)
                    continue;

                var clip = selection as AnimationClip;
                clips.Add(clip);
            }

            if (clips.Count <= 0)
                return;

            var core = new Core();
            var option = new Option();

            foreach (var clip in clips)
                core.Compress(clip, option);
        }

        [MenuItem("测试/打开Animation Compressor")]
        public static void OpenMaterialRefCleaner()
        {
            ShowWindow();
        }

        private static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<Window>("Anim. Compressor");
            window.Show();
        }
    }
}