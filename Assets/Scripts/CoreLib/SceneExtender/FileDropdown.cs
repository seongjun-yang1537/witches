#if UNITY_EDITOR
namespace Corelib.Utils
{
    using System;
    using System.IO;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    public class FileDropdown : AdvancedDropdown
    {
        /// <summary>
        /// Specifies the name of the top-level node. If not set, the root directory name will be used.
        /// </summary>
        public string rootName;

        /// <summary>
        /// An optional filter to apply to files, e.g. *.txt to only show text files.
        /// </summary>
        public string fileFilter;

        private readonly DirectoryInfo rootDirectory;
        private readonly Action<CallbackInfo> onFileSelected;
        private readonly Action<CallbackInfo, object> onFileSelectedAdvanced;
        private readonly object userData;

        public FileDropdown(AdvancedDropdownState state, string directoryPath, Action<CallbackInfo> onFileSelected)
            : this(state, directoryPath)
        {
            this.onFileSelected = onFileSelected;
        }

        public FileDropdown(AdvancedDropdownState state, string directoryPath, Action<CallbackInfo, object> onFileSelected, object userData)
            : this(state, directoryPath)
        {
            this.onFileSelectedAdvanced = onFileSelected;
            this.userData = userData;
        }

        private FileDropdown(AdvancedDropdownState state, string directoryPath) : base(state)
        {
            this.minimumSize = new Vector2(200, 300);
            this.rootDirectory = new DirectoryInfo(directoryPath);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            if (string.IsNullOrEmpty(rootName))
                rootName = rootDirectory.Name;

            var root = new AdvancedDropdownItem(rootName);
            AddFileSystemEntries(root, rootDirectory);
            return root;
        }

        private void AddFileSystemEntries(AdvancedDropdownItem root, DirectoryInfo directory)
        {
            foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
            {
                var folder = new FileDropdownItem(subDirectory.Name, subDirectory.FullName);
                folder.AddChild(new FileDropdownItem("Select Folder", subDirectory.FullName));
                AddFileSystemEntries(folder, subDirectory);
                root.AddChild(folder);
            }

            foreach (FileInfo file in directory.EnumerateFiles(fileFilter, SearchOption.TopDirectoryOnly))
            {
                var child = new FileDropdownItem(file.Name, file.FullName);
                root.AddChild(child);
            }
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            var fileItem = (FileDropdownItem)item;
            var info = new CallbackInfo(fileItem.name, fileItem.fullName);

            onFileSelected?.Invoke(info);
            onFileSelectedAdvanced?.Invoke(info, userData);
        }

        private class FileDropdownItem : AdvancedDropdownItem
        {
            public readonly string fullName;

            public FileDropdownItem(string name, string fullName) : base(name)
            {
                this.fullName = fullName.Replace(@"\", "/");
            }
        }

        /// <summary>
        /// Provides information about the selected file or directory.
        /// </summary>
        public struct CallbackInfo
        {
            /// <summary>
            /// The name of the file (including its extension) or directory.
            /// </summary>
            public readonly string name;

            /// <summary>
            /// The full path of the file or directory.
            /// </summary>
            public readonly string fullName;

            public CallbackInfo(string name, string fullName)
            {
                this.name = name;
                this.fullName = fullName;
            }
        }
    }
}
#endif