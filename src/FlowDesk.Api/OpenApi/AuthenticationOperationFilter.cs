using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FlowDesk.Api.OpenApi;

public sealed class AuthenticationOperationFilter
    : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        object[] controllerAttributes =
            context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
            ?? [];

        object[] actionAttributes =
            context.MethodInfo.GetCustomAttributes(true);

        IEnumerable<object> attributes =
            controllerAttributes.Concat(
                actionAttributes);

        bool allowsAnonymous =
            attributes.OfType<IAllowAnonymous>().Any();

        bool requiresAuthorization =
            attributes.OfType<IAuthorizeData>().Any();

        if (allowsAnonymous || !requiresAuthorization)
        {
            operation.Security = [];
        }
    }
}
