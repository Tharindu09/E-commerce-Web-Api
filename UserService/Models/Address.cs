using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

public class Address
{   
    [Key]
    public int Id { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string Country { get; set; }
    public string Phone { get; set; }

    public string PostalCode { get; set; }

    //navigation
    public User User{ get; set; }
}
