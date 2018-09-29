using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyRateProvider.Common.DAL.Entities
{
    /// <summary>
    /// Валюта
    /// </summary>
    [Table("currency")]
    public class Currency
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        [Column("code")]
        [MaxLength(3)]
        public string Code { get; set; }

        /// <summary>
        /// Количество (1, 10, 100, 1000...), используемое на торгах
        /// </summary>
        [Column("amount")]
        public int Amount { get; set; }
    }
}