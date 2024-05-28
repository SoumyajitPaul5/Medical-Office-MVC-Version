namespace MedicalOffice.ViewModels
{
    public class CheckOptionVM
    {
        // ID of the option, used for identifying the checkbox
        public int ID { get; set; }

        // Text displayed next to the checkbox
        public string DisplayText { get; set; }

        // Indicates whether the option is assigned (checked)
        public bool Assigned { get; set; }
    }
}
