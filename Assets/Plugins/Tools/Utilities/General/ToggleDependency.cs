using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Other
{
    public class ToggleDependency : MonoBehaviour
    {
        public List<Behaviour> syncEnabledStateList;
        public List<GameObject> syncActiveStateList;

        public List<Behaviour> syncDisabledStateList;
        public List<GameObject> syncInactiveStateList;

        protected void OnEnable()
        {
            foreach(var script in syncEnabledStateList)
            {
                script.enabled = true;
            }

            foreach(var gobj in syncActiveStateList)
            {
                gobj.SetActive(true);
            }

            foreach(var script in syncDisabledStateList)
            {
                script.enabled = false;
            }

            foreach(var gobj in syncInactiveStateList)
            {
                gobj.SetActive(false);
            }
        }

        protected void OnDisable()
        {
            foreach(var script in syncEnabledStateList)
            {
                script.enabled = false;
            }

            foreach(var gobj in syncActiveStateList)
            {
                gobj.SetActive(false);
            }

            foreach(var script in syncDisabledStateList)
            {
                script.enabled = true;
            }

            foreach(var gobj in syncInactiveStateList)
            {
                gobj.SetActive(true);
            }
        }
    }
}
