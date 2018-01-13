using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    public class TR_JudgmentQuestionDisplay
    {
        public string QuestionDescripe { get; set; }
        public string Answer { get; set; }
        public string TrueAnswer { get; set; }
        public int IsTrue { get; set; }
    }
}
