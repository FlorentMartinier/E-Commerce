//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace sansNom.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Commande
    {
        public int IdCommande { get; set; }
        public int idClient { get; set; }
        public System.DateTime dateComande { get; set; }
        public string etat { get; set; }
        public string livraisonAdr { get; set; }
    }
}
