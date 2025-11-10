using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace CallCenter.Web.Helpers
{
    public static class AppUiHelpers
    {
        public static void LockActionsByStatus(
            string status,
            Button btnResolve, Button btnClose, Button btnAssign, DropDownList ddlAssign, 
            HtmlButton btnToggleResolve, HtmlButton btnToggleClose
           )
        {
            bool isLocked = IsLocked(status);

            if (btnResolve != null) { btnResolve.Enabled = !isLocked; if (isLocked) btnResolve.CssClass += " disabled"; }
            if (btnClose != null) { btnClose.Enabled = !isLocked; if (isLocked) btnClose.CssClass += " disabled"; }
            if (btnAssign != null) { btnAssign.Enabled = !isLocked; if (isLocked) btnAssign.CssClass += " disabled"; }
            if (ddlAssign != null) ddlAssign.Enabled = !isLocked;

            if (btnToggleResolve != null)
            {
                btnToggleResolve.Disabled = isLocked;
                if (isLocked) btnToggleResolve.Attributes["class"] = (btnToggleResolve.Attributes["class"] ?? "") + " disabled";
            }
            if (btnToggleClose != null)
            {
                btnToggleClose.Disabled = isLocked;
                if (isLocked) btnToggleClose.Attributes["class"] = (btnToggleClose.Attributes["class"] ?? "") + " disabled";
            }
           
        }

        public static bool IsLocked(string status)
        {
            if (status == null) return false;
            return status.Equals("Resuelto", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Cerrado", StringComparison.OrdinalIgnoreCase);
        }

        public static string StatusBadgeClass(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "badge bg-secondary";
            switch (status.Trim().ToLowerInvariant())
            {
                case "abierto": return "badge bg-primary";
                case "asignado": return "badge bg-info text-dark";
                case "en análisis": return "badge bg-warning text-dark";
                case "resuelto": return "badge bg-success";
                case "cerrado": return "badge bg-danger";
                case "reabierto": return "badge bg-dark";
                default: return "badge bg-secondary";
            }
        }
    }
}
