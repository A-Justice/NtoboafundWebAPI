﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NtoboaFund.Data.Models
{
    public class Scholarship
    {
        public int Id { get; set; }

        //The Amount Staked
        public decimal Amount { get; set; }

        //Date the bet was staked
        public string Date { get; set; }

        //The Period of the bet
        public string Period { get; set; }

        //Status of the bet ie. Pending, Paid, Wonder
        public string Status { get; set; }

        //Potential Amount To Win
        public decimal AmountToWin { get; set; }

        //Date The Winner was Declared
        public string DateDeclared { get; set; }

        [Required]
        public string Institution { get; set; }

        [Required]
        public string Program { get; set; }

        [Required]
        public string StudentId { get; set; }

        public int? TransferId { get; set; }

        public string TxRef { get; set; }


        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
