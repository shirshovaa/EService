//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EService.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public partial class TypeModel : INotifyPropertyChanged
    {
        long rowid;
        string fullName;
        string shortName;
        ICollection<Model> models;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TypeModel()
        {
            this.Models = new HashSet<Model>();
        }
    
        public long Rowid { get { return rowid; } set { rowid = value; OnPropertyChanged("Rowid"); } }
        public string FullName { get { return fullName; } set { fullName = value; OnPropertyChanged("FullName"); } }
        public string ShortName { get { return shortName; } set { shortName = value; OnPropertyChanged("ShortName"); } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Model> Models { get { return models; } set { models = value; OnPropertyChanged("Models"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
