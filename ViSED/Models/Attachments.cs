
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
    using System.Collections.Generic;
    
public partial class Attachments
{

    public int id { get; set; }

    public int id_message { get; set; }

    public string attachedFile { get; set; }



    public virtual Message Message { get; set; }

}

}
