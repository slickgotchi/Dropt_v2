using System;
using System.Reflection;
using CarlosLab.Common;
using CarlosLab.UtilityIntelligence.UI;

namespace CarlosLab.UtilityIntelligence
{
    public static class GenericModelFactory
    {
        public static TModelWithId CreateWithId<TModelWithId>(Type runtimeType) where TModelWithId : GenericModel, IGenericModel, IModelWithId, new()
        {
            TModelWithId model = Create<TModelWithId>(runtimeType);
            model.Id = Guid.NewGuid().ToString();
            return model;
        }
        
        public static TModel Create<TModel>(Type runtimeType) where TModel : GenericModel, IGenericModel, new()
        {
            TModel model = new();
            model.Init(runtimeType);

            Blackboard blackboard = UtilityIntelligenceEditorUtils.Blackboard?.Model.Runtime;

            if (blackboard == null) return model;
            
            var variableReferenceFields = model.VariableReferenceFields;
            for (int index = 0; index < variableReferenceFields.Count; index++)
            {
                FieldInfo fieldInfo = variableReferenceFields[index];
                object fieldValue = model.GetValue(fieldInfo.Name);

                if (fieldValue is IVariableReference variableReference) variableReference.Blackboard = blackboard;
            }

            return model;
        }
    }
}