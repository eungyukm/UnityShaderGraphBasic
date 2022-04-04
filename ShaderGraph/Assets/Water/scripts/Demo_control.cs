using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace EasyGameStudio.Jeremy
{
    public class Demo_control : MonoBehaviour
    {

        public int index;

        public Material[] materials;

        public Renderer render;

        // Start is called before the first frame update
        void Start()
        {
            this.change_to_index();
        }
        public void on_next_btn()
        {
            this.index++;
            if (this.index >= this.materials.Length)
                this.index = 0;

            this.change_to_index();
        }
        public void on_previous_btn()
        {
            this.index--;
            if (this.index < 0)
                this.index = this.materials.Length - 1;

            this.change_to_index();
        }
        private void change_to_index()
        {
            this.render.material = this.materials[this.index];
        }
    }
}