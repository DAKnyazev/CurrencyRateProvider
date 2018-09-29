﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CurrencyRateProvider.Common.DAL.Entities
{
    /// <summary>
    /// Дневной курс валюты
    /// </summary>
    [Table("rate")]
    public class Rate
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Относительная валюта
        /// </summary>
        [Column("relative_currency_id")]
        [ForeignKey("RelativeCurrency")]
        public short RelativeCurrencyId { get; set; }
        public Currency RelativeCurrency { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        [Column("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Стоимость
        /// </summary>
        [Column("cost")]
        public decimal Cost { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        [Column("currency_id")]
        [ForeignKey("Currency")]
        public short CurrencyId { get; set; }
        public Currency Currency { get; set; }
    }
}
