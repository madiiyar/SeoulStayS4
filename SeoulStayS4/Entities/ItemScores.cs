//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SeoulStayS4.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class ItemScores
    {
        public long ID { get; set; }
        public System.Guid GUID { get; set; }
        public long UserID { get; set; }
        public long ItemID { get; set; }
        public long ScoreID { get; set; }
        public long Value { get; set; }
    
        public virtual Items Items { get; set; }
        public virtual Scores Scores { get; set; }
        public virtual Users Users { get; set; }
    }
}
