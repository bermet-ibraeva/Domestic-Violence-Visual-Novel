// public class NotesBadgeUI : MonoBehaviour
// {
//     [SerializeField] private GameObject badgeRoot;
//     [SerializeField] private TMPro.TextMeshProUGUI badgeText;

//     private void OnEnable()
//     {
//         UpdateBadge();
//     }

//     public void UpdateBadge()
//     {
//         var save = SaveSystem.Load();

//         if (save == null)
//             return;

//         int count = save.GetUnreadNotesCount();

//         if (count > 0)
//         {
//             badgeRoot.SetActive(true);
//             badgeText.text = count.ToString();
//         }
//         else
//         {
//             badgeRoot.SetActive(false);
//         }
//     }
// }