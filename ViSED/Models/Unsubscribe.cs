
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
    
public partial class Unsubscribe
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Unsubscribe()
    {

        this.UnsubAttachments = new HashSet<UnsubAttachments>();

    }


    public int id { get; set; }

    public int from_user_id { get; set; }

    public int to_user_id { get; set; }

    public System.DateTime date_of_unsub { get; set; }

    public System.DateTime date_of_execution { get; set; }

    public bool executed { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<UnsubAttachments> UnsubAttachments { get; set; }

    public virtual Users Users { get; set; }

    public virtual Users Users1 { get; set; }

}

}
