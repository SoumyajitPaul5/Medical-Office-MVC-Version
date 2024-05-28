namespace MedicalOffice.Models
{
    // Interface for auditing fields
    internal interface IAuditable
    {
        // Property to store the user who created the entity
        string CreatedBy { get; set; }

        // Property to store the creation date and time of the entity
        DateTime? CreatedOn { get; set; }

        // Property to store the user who last updated the entity
        string UpdatedBy { get; set; }

        // Property to store the last update date and time of the entity
        DateTime? UpdatedOn { get; set; }
    }
}
