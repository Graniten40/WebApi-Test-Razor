using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using Models;

namespace Models.Authorization
{
    public static class CrudOperations
    {
        public static OperationAuthorizationRequirement Create =
            new() { Name = nameof(Create) };
        public static OperationAuthorizationRequirement Read =
            new() { Name = nameof(Read) };
        public static OperationAuthorizationRequirement Edit =
            new() { Name = nameof(Edit) };
        public static OperationAuthorizationRequirement Delete =
            new() { Name = nameof(Delete) };
    }

    public class MusicGroupAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, IMusicGroup>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, 
            OperationAuthorizationRequirement requirement, IMusicGroup resource)
        {
            //Notice I can be more granular here and check for specific roles or claims (context.User) and resource properties
            //Non authenticated users can only read
            if (!context.User.Identity.IsAuthenticated)
            {
                if (requirement.Name == CrudOperations.Read.Name)
                {
                    context.Succeed(requirement);
                }
                return Task.CompletedTask;
            }

            //Authenticated users can create, read, edit, delete
            if (requirement.Name == CrudOperations.Create.Name)
            {
                context.Succeed(requirement);
            }
            if (requirement.Name == CrudOperations.Read.Name)
            {
                context.Succeed(requirement);
            }
            if (requirement.Name == CrudOperations.Edit.Name)
            {
                context.Succeed(requirement);
            }
            if(requirement.Name == CrudOperations.Delete.Name)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
