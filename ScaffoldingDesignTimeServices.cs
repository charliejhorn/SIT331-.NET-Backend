using Microsoft.EntityFrameworkCore.Design;
using HandlebarsDotNet;
using Microsoft.Extensions.DependencyInjection;

public class ScaffoldingDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.AddHandlebarsScaffolding();

        (string helperName, Action<EncodedTextWriter, Context, Arguments> helperFunction) camelCase = 
            ("camel-case", ToCamelCase);

        services.AddHandlebarsHelpers(camelCase);

        services.AddHandlebarsTransformers(
            // entityTypeNameTransformer: n => char.ToLower(n[0]) + n.Substring(1)
            // constructorTransformer: 
        );
    }

    private void ToCamelCase(EncodedTextWriter writer, Context context, Arguments arguments)
    {
        if (arguments.Length > 0 && arguments[0] is string input && !string.IsNullOrEmpty(input))
        {
            string camelCased = char.ToLower(input[0]) + input.Substring(1);
            writer.WriteSafeString(camelCased);
        }
    }
}