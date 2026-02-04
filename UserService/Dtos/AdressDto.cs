namespace UserService.Dtos;

public class AddressDto
{   
    public int UserId { get; set; }
    public string Phone { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string Country { get; set; }
    
    public string PostalCode { get; set; }

}