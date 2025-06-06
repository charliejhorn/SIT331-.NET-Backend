using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace robot_controller_api.Models;

[Table("robot_command")]
public class RobotCommand
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    [Column("description")]
    [StringLength(800)]
    public string? Description { get; set; }
    [Column("is_move_command")]
    public bool IsMoveCommand { get; set; }
    [Column("created_date")]
    public DateTime CreatedDate { get; set; }
    [Column("modified_date")]
    public DateTime ModifiedDate { get; set; }

    public RobotCommand(){}

    public RobotCommand(
        int id,
        string name,
        string? description,
        bool isMoveCommand,
        DateTime createdDate,
        DateTime modifiedDate
    )
    {
        this.Id = id;
        this.Name = name;
        this.Description = description;
        this.IsMoveCommand = isMoveCommand;
        this.CreatedDate = createdDate;
        this.ModifiedDate = modifiedDate;
    }

}
