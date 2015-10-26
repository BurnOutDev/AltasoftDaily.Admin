using AltasoftDaily.Core;
using AltasoftDaily.Domain;
using AltasoftDaily.Domain.POCO;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AltasoftDaily.Admin
{
    public partial class FormAdmin : MetroForm
    {
        private AltasoftDailyContext _db;

        public AltasoftDailyContext Db
        {
            get
            {
                if (_db == null)
                    _db = new AltasoftDailyContext();
                return _db;
            }
        }

        public User SelectedUser
        {
            get
            {
                dynamic value = lbUsers.Items[lbUsers.SelectedIndex];

                int key = value.Key;

                return Db.Users.FirstOrDefault(x => x.AltasoftUserID == key);
            }
        }

        public FormAdmin()
        {
            InitializeComponent();
            InitializeFormCustom();
        }

        private void InitializeFormCustom()
        {
            #region Add Users To Dictionary From Database
            var usersDictionary = new Dictionary<int, string>();
            Db.Users.ToList().ForEach(x => usersDictionary.Add(x.AltasoftUserID, x.Name + " " + x.LastName));
            #endregion

            #region Add Depts To Dictionary
            var deptsDictionary = new Dictionary<int, string>();

            deptsDictionary.Add(0, "შპს ბიზნეს კრედიტი");
            deptsDictionary.Add(1, "ცენტრალური სერვის ცენტრი");
            deptsDictionary.Add(2, "ისნის სერვის ცენტრი");
            deptsDictionary.Add(3, "ოკრიბა სერვის ცენტრი");
            deptsDictionary.Add(4, "ლილოს სერვის ცენტრი");
            deptsDictionary.Add(5, "ელიავა სერვის ცენტრი");
            deptsDictionary.Add(6, "ვაგზლის სერვის ცენტრი");
            deptsDictionary.Add(7, "გლდანის სერვის ცენტრი");
            #endregion

            #region Fill Controls
            clbVisibleUsers.DisplayMember = "Value";
            lbUsers.DisplayMember = "Value";
            clbDepts.DisplayMember = "Value";

            foreach (var user in usersDictionary)
            {
                clbVisibleUsers.Items.Add(user);
                lbUsers.Items.Add(user);
            }

            foreach (var dept in deptsDictionary)
            {
                clbDepts.Items.Add(dept);
            }
            #endregion

            #region Control btnSaveChanges Enabled State
            if (lbUsers.SelectedIndex == -1)
                btnSaveChanges.Enabled = false;
            #endregion
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            SelectedUser.Filter.FilterData.Clear();

            foreach (var item in clbVisibleUsers.CheckedItems)
            {
                dynamic value = item;
                int key = value.Key;
                SelectedUser.Filter.FilterData.Add(new FilterData() { OperatorID = key });
            }

            foreach (var item in clbDepts.CheckedItems)
            {
                dynamic value = item;
                int key = value.Key;
                SelectedUser.Filter.FilterData.Add(new FilterData() { DeptID = key });
            }

            SelectedUser.CanSubmit = cbxCanSubmit.Checked;
            SelectedUser.CanViewDaily = cbxCanViewDaily.Checked;
            SelectedUser.IsLockedOut = cbxBlockedOut.Checked;

            Db.SaveChanges();
        }

        private void lbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region Control btnSaveChanges Enabled State
            if (lbUsers.SelectedIndex > -1)
                btnSaveChanges.Enabled = true;
            else
            {
                btnSaveChanges.Enabled = false;
                return;
            }
            #endregion

            #region Set Checkboxes Checked States By Selected User
            cbxBlockedOut.Checked = SelectedUser.IsLockedOut;
            cbxCanSubmit.Checked = SelectedUser.CanSubmit;
            cbxCanViewDaily.Checked = SelectedUser.CanViewDaily;
            #endregion

            #region Set Check State To All Items in cblVisibleUsers CheckedListBox By Selected User
            for (int i = 0; i < clbVisibleUsers.Items.Count; i++)
                clbVisibleUsers.SetItemCheckState(i, CheckState.Unchecked);

            foreach (var u in SelectedUser.Filter.FilterData.Where(x => x.OperatorID > 0))
            {
                var user = Db.Users.FirstOrDefault(x => x.AltasoftUserID == u.OperatorID);

                var index = clbVisibleUsers.Items.IndexOf(new KeyValuePair<int, string>(user.AltasoftUserID, user.Name + " " + user.LastName));
                clbVisibleUsers.SetItemChecked(index, true);
            }
            #endregion

            #region Set Check State To All Items in cblDepts CheckedListBox By Selected User
            for (int i = 0; i < clbDepts.Items.Count; i++)
                clbDepts.SetItemCheckState(i, CheckState.Unchecked);

            foreach (var d in SelectedUser.Filter.FilterData.Where(x => x.DeptID > 0))
            {
                clbDepts.SetItemChecked(d.DeptID, true);
            } 
            #endregion
        }
    }
}
