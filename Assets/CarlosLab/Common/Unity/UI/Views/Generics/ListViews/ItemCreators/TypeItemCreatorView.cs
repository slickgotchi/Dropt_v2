#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

namespace CarlosLab.Common.UI
{
    public abstract class
        TypeItemCreatorView<TListViewModel, TItemViewModel> : ItemCreatorView<TListViewModel, TItemViewModel>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : IItemViewModel
    {
        private PopupField<Type> typeField;

        private VisualElement typeFieldContainer;

        protected virtual string TypeFieldLabel { get; } = "Type";
        
        protected virtual Type BaseType { get; }
        protected virtual HashSet<Type> ExcludedTypes { get; } = null;
        
        public TypeItemCreatorView(string visualAssetPath = UIBuilderResourcePaths.TypeItemCreatorView) : base(
            visualAssetPath)
        {
        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();

            typeFieldContainer = this.Q<VisualElement>("TypeFieldContainer");

            CreateTypeField(TypeFieldLabel);
            UpdateTypeFieldChoices(BaseType, ExcludedTypes);
        }

        protected PopupField<Type> TypeField => typeField;

        private void CreateTypeField(string label)
        {
            typeField = new()
            {
                label = label
            };
            
            typeField.formatListItemCallback = FormatListItem;
            typeField.formatSelectedValueCallback = FormatSelectedItem;
            
            typeFieldContainer.Add(typeField);
        }

        protected virtual string FormatListItem(Type type)
        {
            if (type == null)
                return "None";

            return type.Name;
        }
        
        protected virtual string FormatSelectedItem(Type type)
        {
            if (type == null)
                return "None";

            return type.Name;
        }

        private void UpdateTypeFieldChoices(Type baseType, HashSet<Type> excludedTypes)
        {
            if (baseType == null) return;
            
            List<Type> types = null;

#if UNITY_EDITOR
            types = TypeCache.GetTypesDerivedFrom(baseType).ToList();
#else
            types = new();
#endif
            types.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            foreach (Type type in types)
            {
                if (type.IsGenericType || type.IsAbstract || (excludedTypes != null && excludedTypes.Contains(type))) continue;

                typeField.choices.Add(type);
            }

            if (typeField.choices.Count > 0 && typeField.value == null)
                typeField.SetValueWithoutNotify(typeField.choices[0]);
        }

        protected override void CreateNewItem()
        {
            Type type = typeField.value;
            if (type != null) ViewModel.CreateItem(type);
        }
    }
}