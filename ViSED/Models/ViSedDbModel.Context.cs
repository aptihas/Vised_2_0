﻿

//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------


namespace ViSED.Models
{

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;


public partial class ViSedDBEntities : DbContext
{
    public ViSedDBEntities()
        : base("name=ViSedDBEntities")
    {

    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        throw new UnintentionalCodeFirstException();
    }


    public virtual DbSet<Accounts> Accounts { get; set; }

    public virtual DbSet<Admins> Admins { get; set; }

    public virtual DbSet<Attachments> Attachments { get; set; }

    public virtual DbSet<Audio> Audio { get; set; }

    public virtual DbSet<DocType> DocType { get; set; }

    public virtual DbSet<Dolgnosti> Dolgnosti { get; set; }

    public virtual DbSet<Letters> Letters { get; set; }

    public virtual DbSet<MyDocs> MyDocs { get; set; }

    public virtual DbSet<Podrazdeleniya> Podrazdeleniya { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<TaskAttachments> TaskAttachments { get; set; }

    public virtual DbSet<Tasks> Tasks { get; set; }

    public virtual DbSet<Users> Users { get; set; }

}

}

