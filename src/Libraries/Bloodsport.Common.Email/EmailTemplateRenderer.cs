using Scriban;

namespace Bloodsport.Common.Email;

public class EmailTemplateRenderer
{
    private static readonly string TemplatesPath = Path.Combine(
        AppContext.BaseDirectory, "EmailTemplates");

    public async Task<string> RenderAsync(string templateName, object model)
    {
        var path = Path.Combine(TemplatesPath, templateName);
        var source = await File.ReadAllTextAsync(path);
        var template = Template.Parse(source);
        return await template.RenderAsync(model, member => member.Name);
    }
}
