namespace MedicalOffice.Models
{
    // Represents the association between a patient and a medical condition
    public class PatientCondition
    {
        // Foreign key for Condition
        public int ConditionID { get; set; }
        // Navigation property for Condition
        public Condition Condition { get; set; }

        // Foreign key for Patient
        public int PatientID { get; set; }
        // Navigation property for Patient
        public Patient Patient { get; set; }
    }
}
