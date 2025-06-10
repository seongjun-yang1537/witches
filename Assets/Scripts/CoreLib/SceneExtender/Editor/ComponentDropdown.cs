using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Corelib.Utils
{
    public class ComponentDropDown : AdvancedDropdown
    {
        public ComponentDropDown(AdvancedDropdownState state) : base(state)
        {
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Root");

            // 서브 항목 추가
            var item1 = new AdvancedDropdownItem("Item 1");
            var item2 = new AdvancedDropdownItem("Item 2");
            var item3 = new AdvancedDropdownItem("Item 3");

            // 서브 항목을 루트에 추가
            root.AddChild(item1);
            root.AddChild(item2);
            root.AddChild(item3);

            return root;
        }
    }
}