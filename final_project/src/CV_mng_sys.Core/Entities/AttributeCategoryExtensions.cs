namespace CV_mng_sys.Core.Entities;

public static class AttributeCategoryExtensions
{
    public static string ToDisplayName(this AttributeCategory category)
    {
        return category switch
        {
            AttributeCategory.PersonalInformation => "Personal Information",
            AttributeCategory.DomainKnowledge => "Domain Knowledge",
            AttributeCategory.SoftSkills => "Soft Skills",
            AttributeCategory.Certification => "Certification",
            AttributeCategory.Other => "Other",
            _ => category.ToString()
        };
    }
}