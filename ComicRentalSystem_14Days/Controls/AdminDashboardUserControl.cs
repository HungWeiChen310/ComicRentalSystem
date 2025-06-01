using System;
using System.Windows.Forms;
using ComicRentalSystem_14Days.Services;
using ComicRentalSystem_14Days.Interfaces; // �Y�ݭn ILogger
using System.Threading.Tasks;

namespace ComicRentalSystem_14Days.Controls
{
    public partial class AdminDashboardUserControl : UserControl
    {
        // �w�w (A) �b�o�̫ŧi Label ���ADesigner ���u��ҤơA���n���ƫŧi �w�w
        internal Label lblTotalComicsValue;
        internal Label lblRentedComicsValue;
        internal Label lblAvailableComicsValue;
        internal Label lblActiveMembersValue;

        private readonly ComicService _comicService;
        private readonly MemberService _memberService;
        private readonly ILogger _logger;

        public AdminDashboardUserControl(
            ComicService comicService,
            MemberService memberService,
            ILogger logger
        )
        {
            _comicService = comicService ?? throw new ArgumentNullException(nameof(comicService));
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeComponent();
        }

        /// <summary>
        /// ����ɩI�s����k�A�q Service ����ƨç�s�|�� Label �� Text
        /// </summary>
        public void LoadDashboardData()
        {
            try
            {
                // (1) �`���e��
                int totalComicsCount = _comicService.GetAllComics()?.Count ?? 0;
                lblTotalComicsValue.Text = totalComicsCount.ToString();

                // (2) �w���ɺ��e��
                int rentedComicsCount = _comicService.GetAllComics().Count(c => c.IsRented);
                lblRentedComicsValue.Text = rentedComicsCount.ToString();

                // (3) �i�ɺ��e��
                int availableComicsCount = totalComicsCount - rentedComicsCount;
                lblAvailableComicsValue.Text = availableComicsCount.ToString();

                // (4) ���D�|���ơ]�d�ҡG���] MemberService �^�ǥu�����D�|���^
                int activeMembersCount = _memberService.GetAllMembers()?.Count ?? 0;
                lblActiveMembersValue.Text = activeMembersCount.ToString();

                _logger.Log($"儀表板資料已載入: 總數={totalComicsCount}, 已租={rentedComicsCount}, 可借={availableComicsCount}, 活躍會員={activeMembersCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError("AdminDashboardUserControl.LoadDashboardData: 讀取指標時發生例外狀況。", ex);
            }
        }
    }
}
