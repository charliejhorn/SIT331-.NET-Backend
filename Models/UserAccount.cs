using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace robot_controller_api.Models;

[Table("user_account")]
public class UserAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("password_hash")]
    [StringLength(200)]
    public string PasswordHash { get; set; } = null!;
    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;
    [Column("description")]
    [StringLength(800)]
    public string? Description { get; set; }
    [Column("first_name")]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;
    [Column("last_name")]
    [StringLength(50)]
    public string LastName { get; set; } = null!;
    [Column("role")]
    [StringLength(20)]
    public string Role { get; set; } = null!;
    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
    [Column("modified_date", TypeName = "timestamp without time zone")]
    public DateTime ModifiedDate { get; set; }

    public UserAccount(){}

    public UserAccount(
        int id,
        string passwordHash,
        string email,
        string? description,
        string firstName,
        string lastName,
        string role,
        DateTime createdDate,
        DateTime modifiedDate
    )
    {
        this.Id = id;
        this.PasswordHash = passwordHash;
        this.Email = email;
        this.Description = description;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.Role = role;
        this.CreatedDate = createdDate;
        this.ModifiedDate = modifiedDate;
    }

}
