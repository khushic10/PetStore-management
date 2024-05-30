namespace Project.Models
{
    public class PetModel
    {
        public int Id { get; set; }  
        public string PetName { get; set;}
        public string PetDescription { get; set;}
        public string PetType { get; set;}
        public string PetBreed { get; set;}
        public int PetCost { get; set;}
        public string PetStatus { get; set; }
        public byte[] PetImage { get; set; }

    }
}
