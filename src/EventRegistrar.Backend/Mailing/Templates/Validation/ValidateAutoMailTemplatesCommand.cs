using System.Text.Json;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.MenuNodes;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Templates.Validation;

public class ValidateAutoMailTemplatesCommand : IRequest
{
    public Guid EventId { get; set; }
    public Guid? AutoMailTemplateId { get; set; }
}

public class ValidateAutoMailTemplatesCommandHandler(IQueryable<AutoMailTemplate> templates,
                                                     IEnumerable<IAutoMailTemplateExpectedPlaceholders> expectedPlaceholders,
                                                     IRepository<MenuNodeReadModel> menuNodes,
                                                     ChangeTrigger changeTrigger)
    : IRequestHandler<ValidateAutoMailTemplatesCommand>
{
    public async Task Handle(ValidateAutoMailTemplatesCommand command, CancellationToken cancellationToken)
    {
        var mailTypesToCheck = expectedPlaceholders.SelectMany(eph => eph.AppliesTo);
        var templatesToCheck = await templates.Where(amt => amt.EventId == command.EventId
                                                         && mailTypesToCheck.Contains(amt.Type))
                                              .WhereIf(command.AutoMailTemplateId != null,
                                                       amt => amt.Id == command.AutoMailTemplateId)
                                              .ToListAsync(cancellationToken);
        foreach (var template in templatesToCheck)
        {
            var content = template.ContentHtml ?? string.Empty;
            template.FailedPlaceholderChecks = expectedPlaceholders.Where(eph => eph.AppliesTo.Contains(template.Type))
                                                                   .Where(eph => eph.MissesPlaceholder(content))
                                                                   .Select(eph => eph.GetType().Name)
                                                                   .ToList();
        }

        var totalWarningCount = templatesToCheck.Sum(amt => amt.FailedPlaceholderChecks?.Count ?? 0);
        var menuNode = new MenuNodeCalculation { Key = MenuNodeKey.MailTemplates };
        if (totalWarningCount > 0)
        {
            menuNode.Content = $"{totalWarningCount}";
            menuNode.Style = MenuNodeStyle.ToDo;
            menuNode.ToolTipData = new MailTemplatesToolTipData { WarningCount = totalWarningCount };
        }
        await UpsertMenuNode(command.EventId, menuNode);
    }

    private async Task UpsertMenuNode(Guid eventId, MenuNodeCalculation menuNodeCalculation)
    {
        var toolTipData = menuNodeCalculation.ToolTipData != null
                              ? JsonSerializer.Serialize(menuNodeCalculation.ToolTipData)
                              : null;
        var toolTipDataType = menuNodeCalculation.ToolTipData?.GetType().FullName;

        var node = await menuNodes.AsTracking()
                                  .FirstOrDefaultAsync(mnr => mnr.EventId == eventId
                                                           && mnr.Key == menuNodeCalculation.Key);
        var anythingChanged = false;
        if (node == null)
        {
            anythingChanged = true;
            menuNodes.InsertObjectTree(new MenuNodeReadModel
                                       {
                                           Id = Guid.NewGuid(),
                                           EventId = eventId,
                                           Key = menuNodeCalculation.Key,
                                           Content = menuNodeCalculation.Content,
                                           Hidden = menuNodeCalculation.Hidden,
                                           ToolTipData = toolTipData,
                                           ToolTipDataType = toolTipDataType
                                       });
        }
        else if (node.Content != menuNodeCalculation.Content
              || node.Style != menuNodeCalculation.Style
              || node.Hidden != menuNodeCalculation.Hidden
              || node.ToolTipData != toolTipData
              || node.ToolTipDataType != toolTipDataType)
        {
            anythingChanged = true;
            node.Content = menuNodeCalculation.Content;
            node.Style = menuNodeCalculation.Style;
            node.Hidden = menuNodeCalculation.Hidden;
            node.ToolTipData = toolTipData;
            node.ToolTipDataType = toolTipDataType;
        }

        if (anythingChanged)
        {
            changeTrigger.QueryChanged<MenuNodesQuery>(eventId);
        }
    }
}

public class MailTemplatesToolTipData
{
    public int WarningCount { get; set; }
}

public interface IAutoMailTemplateExpectedPlaceholders
{
    public MailType[] AppliesTo { get; }
    public bool MissesPlaceholder(string content);
    public string ErrorMessage { get; }
}

public class BothNamesExpectedInPartnerMail : IAutoMailTemplateExpectedPlaceholders
{
    public MailType[] AppliesTo { get; } =
    [
        MailType.PartnerRegistrationMatchedAndAccepted,
        MailType.PartnerRegistrationFullyPaid,
        MailType.PartnerRegistrationFirstReminder,
        MailType.PartnerRegistrationSecondReminder,
        MailType.PartnerRegistrationMatchedOnWaitingList
    ];

    public bool MissesPlaceholder(string content)
    {
        return !content.Contains(GetPlaceholder(Role.Leader, MailPlaceholder.FirstName)) && !content.Contains(GetPlaceholder(Role.Leader, MailPlaceholder.LastName))
            || !content.Contains(GetPlaceholder(Role.Follower, MailPlaceholder.FirstName)) && !content.Contains(GetPlaceholder(Role.Follower, MailPlaceholder.LastName));
    }

    public string ErrorMessage { get; } = string.Format(Resources.BothNamesExpectedInPartnerMail,
                                                        GetPlaceholder(Role.Leader, MailPlaceholder.FirstName),
                                                        GetPlaceholder(Role.Follower, MailPlaceholder.FirstName));

    private static string GetPlaceholder(Role? role, MailPlaceholder placeholder)
    {
        return role == null 
                   ? $"{{{{{placeholder}}}}}" 
                   : $"{{{{{role}.{placeholder}}}}}";
    }
}