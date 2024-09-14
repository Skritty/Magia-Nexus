using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skritty.Tools.Saving;

namespace Skritty.Other
{
    // THIS SCRIPT IS CURRENTLY BEING USED AS A TEST SCRIPT FOR SAVING, IT WILL HAVE ITS FUNCTIONALITY BUILT IN THE FUTURE
    public class ProgressData : ProgressData1
    {
        [SerializeField, SaveLoad]
        private int thing;

        void Start()
        {
            this.Save(SaveFolders.Debug, "test");
            thing = 0;
            test = "no";
            this.Load(SaveFolders.Debug, "test");
        }

        public override string ToString()
        {
            return $"{test}: {thing}";
        }
    }

    public class ProgressData1 : MonoBehaviour
    {
        [SerializeField, SaveLoad]
        protected string test = "test";
    }
}
