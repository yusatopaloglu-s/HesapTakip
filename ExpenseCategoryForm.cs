using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HesapTakip
{
    public partial class ExpenseCategoryForm : Form
    {
        private IDatabaseOperations _db;
        public ExpenseCategoryForm(IDatabaseOperations db)
        {
            InitializeComponent();
            _db = db;
        }
    }
}
