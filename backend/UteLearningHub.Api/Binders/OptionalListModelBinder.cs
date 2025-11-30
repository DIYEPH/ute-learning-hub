using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace UteLearningHub.Api.Binders;

public class OptionalListModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (bindingContext.ModelType == typeof(IList<Guid>))
        {
            var guidList = new List<Guid>();
            var values = valueProviderResult.Values;

            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
                {
                    guidList.Add(guid);
                }
            }

            bindingContext.Result = ModelBindingResult.Success(
                guidList.Count > 0 ? guidList : null
            );
            return Task.CompletedTask;
        }

        if (bindingContext.ModelType == typeof(IList<string>))
        {
            var stringList = new List<string>();
            var values = valueProviderResult.Values;

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    stringList.Add(value.Trim());
                }
            }

            bindingContext.Result = ModelBindingResult.Success(
                stringList.Count > 0 ? stringList : null
            );
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}
