using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Linq;

using Configuration;

namespace Models
{
    public class LatinSentence
    {
        public Guid SentenceId {get; set;} = Guid.NewGuid();
        public string Sentence { get; set; }

        public LatinSentence() {}
        public LatinSentence(LatinSentence original)
        {
            SentenceId = original.SentenceId;
            Sentence = original.Sentence;
        }
    }
}