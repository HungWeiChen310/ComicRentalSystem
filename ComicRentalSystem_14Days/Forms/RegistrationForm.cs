using ComicRentalSystem_14Days.Interfaces;
using ComicRentalSystem_14Days.Models;
using ComicRentalSystem_14Days.Services;
using System;
using System.Linq;
using System.Windows.Forms;

namespace ComicRentalSystem_14Days.Forms
{
    public partial class RegistrationForm : BaseForm 
    {
        private readonly ILogger _logger; 
        private readonly AuthenticationService _authService;
        private readonly MemberService _memberService;
        private readonly User? _currentUser;
        private System.Windows.Forms.ErrorProvider errorProvider1;

        public RegistrationForm(ILogger logger, AuthenticationService authService, MemberService memberService, User? currentUser = null)
        {
            InitializeComponent(); 

            this.errorProvider1 = new System.Windows.Forms.ErrorProvider();

            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            base.SetLogger(this._logger); 

            if (btnRegister != null) StyleModernButton(btnRegister);
            Control[] foundControls = this.Controls.Find("gbAccountCredentials", true);
            if (foundControls.Length > 0 && foundControls[0] is GroupBox gbAcc) StyleModernGroupBox(gbAcc);

            foundControls = this.Controls.Find("gbMemberInfo", true);
            if (foundControls.Length > 0 && foundControls[0] is GroupBox gbInfo) StyleModernGroupBox(gbInfo);


            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _currentUser = currentUser;

            txtPassword.PasswordChar = '*';
            txtConfirmPassword.PasswordChar = '*';
            _logger.Log("註冊表單已初始化。");
        }

        private bool ValidateForm()
        {
            lblSummaryError.Visible = false;
            bool valid = this.ValidateChildren();
            if (!valid)
            {
                lblSummaryError.Text = "請修正標示的錯誤後再提交";
                lblSummaryError.Visible = true;
            }
            return valid;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                MessageBox.Show("請修正欄位中提示的錯誤。", "驗證錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _logger.Log("註冊嘗試因驗證錯誤而失敗。");
                return;
            }

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string name = txtName.Text.Trim();
            string phoneNumber = txtPhoneNumber.Text.Trim();

            UserRole selectedRole;
            if (cmbRole.Visible && cmbRole.Enabled && cmbRole.SelectedItem != null)
            {
                selectedRole = (UserRole)cmbRole.SelectedItem;
            }
            else
            {
                selectedRole = UserRole.Member; 
            }

            _logger.Log($"使用者註冊嘗試: {username}, 姓名: {name}, 電話: {phoneNumber}, 角色: {selectedRole}");
            bool success = _authService.Register(username, password, selectedRole);

            if (success)
            {
                _logger.Log($"使用者 '{username}' (來自txtUsername) 已成功註冊為 {selectedRole}。");
                try
                {
                    Member newMember = new Member { Name = name, PhoneNumber = phoneNumber, Username = username };
                    _memberService.AddMember(newMember);
                    _logger.Log($"已為姓名: {name}, 使用者名稱: {username}, 電話: {phoneNumber} 建立會員記錄。");

                    MessageBox.Show($"使用者 '{username}' (姓名: {name}) 已成功註冊，會員資料也已建立，將返回登入頁面。", "註冊成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtUsername.Clear();
                    txtName.Clear();
                    txtPhoneNumber.Clear();
                    txtPassword.Clear();
                    txtConfirmPassword.Clear();
                    lblSummaryError.Visible = false;
                    this.Close(); // 關閉註冊表單並返回登入頁面
                }
                catch (Exception ex)
                {
                    _logger.LogError($"成功註冊使用者 '{username}' 但建立會員記錄失敗。錯誤: {ex.Message}. 正在嘗試復原使用者註冊。", ex);
                    bool rollbackSuccess = _authService.DeleteUser(username);
                    if (rollbackSuccess)
                    {
                        _logger.Log($"已成功復原 (刪除) 使用者帳戶 '{username}'。");
                    }
                    else
                    {
                        _logger.LogError($"無法復原 (刪除) 使用者帳戶 '{username}'。系統可能處於不一致狀態。");
                    }
                    MessageBox.Show($"註冊過程中發生錯誤，無法建立完整的會員資料。請重試或聯繫管理員。\n錯誤: {ex.Message}", "註冊失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtConfirmPassword.Clear();
                }
            }
            else
            {
                _logger.Log($"使用者 '{username}' 註冊失敗。使用者名稱可能已存在。");
                if (_authService.GetUserByUsername(username) != null)
                {
                    MessageBox.Show("使用者名稱已被使用，請換一個試試。", "註冊失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("無法完成註冊，請稍後再試。", "註冊失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                txtUsername.Clear();
            }
        }

        private void RegistrationForm_Load(object sender, EventArgs e)
        {
            _logger.Log("註冊表單已載入。");
            cmbRole.DataSource = Enum.GetValues(typeof(UserRole));

            if (_currentUser != null && _currentUser.Role == UserRole.Admin)
            {
                lblRole.Visible = true;
                cmbRole.Visible = true;
                cmbRole.Enabled = true;
                cmbRole.SelectedItem = UserRole.Member; 
                _logger.Log("註冊表單由管理員載入。角色選擇已啟用。");
            }
            else
            {
                lblRole.Visible = false;
                cmbRole.Visible = false;
                cmbRole.Enabled = false;
                _logger.Log("註冊表單由非管理員或透過登入表單載入。角色選擇已停用。");
            }
        }

        private void txtUsername_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is TextBox txt && string.IsNullOrWhiteSpace(txt.Text))
            { errorProvider1?.SetError(txt, "使用者名稱不能為空。"); e.Cancel = true; }
            else if (sender is TextBox txtBox) { errorProvider1?.SetError(txtBox, ""); }
        }

        private void txtPassword_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is TextBox txt && string.IsNullOrWhiteSpace(txt.Text))
            { errorProvider1?.SetError(txt, "密碼不能為空。"); e.Cancel = true; }
            else if (sender is TextBox txtP && txtP.Text.Length < 6)
            { errorProvider1?.SetError(txtP, "密碼長度至少需6個字元。"); e.Cancel = true; }
            else if (sender is TextBox txtBox) { errorProvider1?.SetError(txtBox, ""); }
        }

        private void txtConfirmPassword_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is TextBox txt && txt.Text != txtPassword.Text)
            { errorProvider1?.SetError(txt, "密碼不相符。"); e.Cancel = true; }
            else if (sender is TextBox txtB) { errorProvider1?.SetError(txtB, ""); }
        }

        private void txtName_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is TextBox txt)
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                { errorProvider1?.SetError(txt, "姓名不能為空。"); e.Cancel = true; }
                else if (!txt.Text.Trim().All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                { errorProvider1?.SetError(txt, "姓名只能包含字母和空格。"); e.Cancel = true; }
                else { errorProvider1?.SetError(txt, ""); }
            }
        }

        private void txtPhoneNumber_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is TextBox txt)
            {
                string phoneNumber = txt.Text.Trim();
                if (string.IsNullOrWhiteSpace(phoneNumber))
                { errorProvider1?.SetError(txt, "電話號碼不能為空。"); e.Cancel = true; }
                else if (!phoneNumber.All(char.IsDigit))
                { errorProvider1?.SetError(txt, "電話號碼只能包含數字。"); e.Cancel = true; }
                else if (phoneNumber.Length < 7 || phoneNumber.Length > 15)
                { errorProvider1?.SetError(txt, "電話號碼長度必須介於7到15位數字之間。"); e.Cancel = true; }
                else { errorProvider1?.SetError(txt, ""); }
            }
        }
    }
}
