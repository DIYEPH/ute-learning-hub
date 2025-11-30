using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UteLearningHub.Api.Binders;

public class OptionalListModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var modelType = context.Metadata.ModelType;

        if (modelType == typeof(IList<Guid>))
        {
            return new OptionalListModelBinder();
        }

        if (modelType == typeof(IList<string>))
        {
            return new OptionalListModelBinder();
        }

        return null;
    }
}
