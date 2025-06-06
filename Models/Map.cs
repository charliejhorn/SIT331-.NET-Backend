using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace robot_controller_api.Models;

[Table("map")]
public class Map
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("columns")]
    public int Columns { get; set; }
    [Column("rows")]
    public int Rows { get; set; }
    [Column("is_square")]
    public bool? IsSquare { get; set; }
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    [Column("description")]
    [StringLength(800)]
    public string? Description { get; set; }
    [Column("created_date")]
    public DateTime CreatedDate { get; set; }
    [Column("modified_date")]
    public DateTime ModifiedDate { get; set; }

    public Map(){}

    public Map(
        int id,
        int columns,
        int rows,
        bool? isSquare,
        string name,
        string? description,
        DateTime createdDate,
        DateTime modifiedDate
    )
    {
        this.Id = id;
        this.Columns = columns;
        this.Rows = rows;
        this.IsSquare = isSquare;
        this.Name = name;
        this.Description = description;
        this.CreatedDate = createdDate;
        this.ModifiedDate = modifiedDate;
    }

}
