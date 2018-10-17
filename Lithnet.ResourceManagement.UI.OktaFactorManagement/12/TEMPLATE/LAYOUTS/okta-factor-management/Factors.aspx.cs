using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Lithnet.ResourceManagement.Client;
using SD = System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Lithnet.ResourceManagement.UI.OktaFactorManagement
{
    public partial class Factors : System.Web.UI.Page
    {
        private const string EventSourceName = "LithnetOktaFactorManagement";

        private static SD.EventLog eventLog;

        private string UserDisplayName
        {
            get => (string)this.ViewState[nameof(this.UserDisplayName)];
            set => this.ViewState[nameof(this.UserDisplayName)] = value;
        }

        private string UserOktaID
        {
            get => (string)this.ViewState[nameof(this.UserOktaID)];
            set => this.ViewState[nameof(this.UserOktaID)] = value;
        }

        private bool HasReadPermission
        {
            get => (bool)this.ViewState[nameof(this.HasReadPermission)];
            set => this.ViewState[nameof(this.HasReadPermission)] = value;
        }

        private bool HasWritePermission
        {
            get => (bool)this.ViewState[nameof(this.HasWritePermission)];
            set => this.ViewState[nameof(this.HasWritePermission)] = value;
        }

        private string EnrolledFactorsRaw
        {
            get => (string)this.ViewState[nameof(this.EnrolledFactorsRaw)];
            set => this.ViewState[nameof(this.EnrolledFactorsRaw)] = value;
        }

        private string AvailableFactorsRaw
        {
            get => (string)this.ViewState[nameof(this.AvailableFactorsRaw)];
            set => this.ViewState[nameof(this.AvailableFactorsRaw)] = value;
        }

        private string UserObjectID => this.Request.QueryString["id"];

        protected void Page_Init(object sender, EventArgs e)
        {
            //this.ViewStateUserKey = this.Session.SessionID; 
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                SD.Trace.WriteLine($"Loading page. IsPostBack: {this.Page.IsPostBack}. IsPartialPostBack: {System.Web.UI.ScriptManager.GetCurrent(this.Page)?.IsInAsyncPostBack}");
                SD.Trace.WriteLine($"Loaded page as {System.Threading.Thread.CurrentPrincipal.Identity.Name} using {System.Threading.Thread.CurrentPrincipal.Identity.AuthenticationType} authentication");

                if (!this.Page.IsPostBack || this.UserOktaID == null)
                {
                    if (this.LoadUserAndFactorsIntoViewState())
                    {
                        return;
                    }
                }

                this.PopulateUserTable();
                this.PopulateFactorTable();
            }
            catch (ResourceNotFoundException)
            {
                this.LogEvent($"{System.Threading.Thread.CurrentPrincipal.Identity.Name} requested factors for {this.UserObjectID} but an object with that ID was not found in the MIM service", SD.EventLogEntryType.Error, 5);
                this.SetError((string)this.GetLocalResourceObject("ErrorUserNotFound"));
            }
            catch (Exception ex)
            {
                this.LogEvent($"Unexpected error in app\n{ex}", SD.EventLogEntryType.Error, 6);
                SD.Trace.WriteLine($"Exception in page_load\n {ex}");
                this.SetError("An unexpected error occurred:\n" + ex);
            }
        }

        private void PopulateUserTable()
        {
            this.userInfoTable.Rows.Clear();
            this.AddRowToUserTable(this.UserDisplayName);
        }

        private bool LoadUserAndFactorsIntoViewState()
        {
            this.resultRow.Visible = false;
            this.divWarning.Visible = false;

            ResourceObject o = this.GetResource();

            if (o == null)
            {
                this.LogEvent($"{System.Threading.Thread.CurrentPrincipal.Identity.Name} requested factors for {this.UserObjectID} but an object with that ID was not found in the MIM service", SD.EventLogEntryType.Error, 5);
                this.SetError((string)this.GetLocalResourceObject("ErrorUserNotFound"));
                return true;
            }
            else
            {
                SD.Trace.WriteLine($"Got resource {o.ObjectID} from resource management service");
            }

            if (!this.HasReadPermission)
            {
                this.LogEvent($"{System.Threading.Thread.CurrentPrincipal.Identity.Name} requested factors for {this.UserDisplayName} ({this.UserOktaID}) but does not have permission to do so", SD.EventLogEntryType.FailureAudit, 4);
                this.SetError((string)this.GetLocalResourceObject("AccessDenied"));
                return true;
            }

            this.GetEnrolledFactors();

            this.LogEvent($"{System.Threading.Thread.CurrentPrincipal.Identity.Name} requested factors for {this.UserDisplayName} ({this.UserOktaID})", SD.EventLogEntryType.SuccessAudit, 3);

            return false;
        }

        private void GetEnrolledFactors()
        {
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"SSWS {AppConfigurationSection.CurrentConfig.OktaApiKey}");
            client.Headers.Add("User-Agent", "LithnetOktaFactorManagement");
            client.Headers.Add("Accept", "application/json");
            client.Headers.Add("Content-Type", "application/json");

            string response = client.DownloadString($"{AppConfigurationSection.CurrentConfig.OktaDomain}/api/v1/users/{this.UserOktaID}/factors");
            this.EnrolledFactorsRaw = response;

            response = client.DownloadString($"{AppConfigurationSection.CurrentConfig.OktaDomain}/api/v1/users/{this.UserOktaID}/factors/catalog");
            this.AvailableFactorsRaw = response;
        }

        private void ResetFactors(IEnumerable<string> factorIDs, string displayName)
        {
            foreach (string factorID in factorIDs)
            {
                this.ResetFactor(factorID, displayName);
            }
        }

        private void ResetFactor(string factorID, string displayName)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{AppConfigurationSection.CurrentConfig.OktaDomain}/api/v1/users/{this.UserOktaID}/factors/{factorID}");

                request.Method = "DELETE";
                request.Headers.Add("Authorization", $"SSWS {AppConfigurationSection.CurrentConfig.OktaApiKey}");
                request.UserAgent = "LithnetOktaFactorManagement";
                request.Accept = "application/json";
                request.ContentType = "application/json";

                request.GetResponse();

                this.LogEvent($"Factor {displayName} ({factorID}) for user {this.UserDisplayName} ({this.UserOktaID}) was reset by {System.Threading.Thread.CurrentPrincipal.Identity.Name}", SD.EventLogEntryType.SuccessAudit, 1);
            }
            catch (Exception ex)
            {
                this.LogEvent($"Factor {displayName} ({factorID}) for user {this.UserDisplayName} ({this.UserOktaID}) failed to be reset by {System.Threading.Thread.CurrentPrincipal.Identity.Name}.\n{ex}", SD.EventLogEntryType.Error, 2);
                throw;
            }
        }

        private void LogEvent(string message, SD.EventLogEntryType type, int id)
        {
            if (!SD.EventLog.SourceExists(EventSourceName))
            {
                return;
            }

            if (eventLog == null)
            {
                eventLog = new SD.EventLog("Application", ".", EventSourceName);
            }

            eventLog.WriteEntry(message, type, id);
        }

        private ResourceObject GetResource()
        {
            ResourceManagementClient c = new ResourceManagementClient();

            ResourceObject o = c.GetResource(this.UserObjectID, this.GetRmcAttributesToGet(), true);

            this.HasReadPermission = string.IsNullOrEmpty(AppConfigurationSection.CurrentConfig.ReadFactorsAuthZAttributeName) ||
                                     o.Attributes[AppConfigurationSection.CurrentConfig.ReadFactorsAuthZAttributeName].PermissionHint.HasFlag(AttributePermission.Read);

            this.HasWritePermission = !string.IsNullOrEmpty(AppConfigurationSection.CurrentConfig.WriteFactorsAuthZAttributeName) ||
                                      o.Attributes[AppConfigurationSection.CurrentConfig.WriteFactorsAuthZAttributeName].PermissionHint.HasFlag(AttributePermission.Modify);

            this.UserDisplayName = o.Attributes[AppConfigurationSection.CurrentConfig.DisplayNameAttributeName].StringValue;

            this.UserOktaID = o.Attributes[AppConfigurationSection.CurrentConfig.OktaIDAttributeName]?.StringValue;

            return o;
        }

        private IEnumerable<string> GetRmcAttributesToGet()
        {
            if (!string.IsNullOrWhiteSpace(AppConfigurationSection.CurrentConfig.DisplayNameAttributeName))
            {
                yield return AppConfigurationSection.CurrentConfig.DisplayNameAttributeName;
            }

            if (!string.IsNullOrWhiteSpace(AppConfigurationSection.CurrentConfig.OktaIDAttributeName))
            {
                yield return AppConfigurationSection.CurrentConfig.OktaIDAttributeName;
            }

            if (!string.IsNullOrWhiteSpace(AppConfigurationSection.CurrentConfig.ReadFactorsAuthZAttributeName))
            {
                yield return AppConfigurationSection.CurrentConfig.ReadFactorsAuthZAttributeName;
            }

            if (!string.IsNullOrWhiteSpace(AppConfigurationSection.CurrentConfig.WriteFactorsAuthZAttributeName))
            {
                yield return AppConfigurationSection.CurrentConfig.WriteFactorsAuthZAttributeName;
            }
        }

        private void SetError(string message)
        {
            SD.Trace.WriteLine($"Setting error message: {message}");
            this.divWarning.Visible = true;
            this.lbWarning.Text = message;
            this.resultRow.Visible = true;
            this.divPasswordSetMessage.Visible = false;
            this.up2.Update();
        }

        private void PopulateFactorTable()
        {
            this.SetupFactorsTable();

            JArray value = JArray.Parse(this.EnrolledFactorsRaw);

            List<OktaFactor> factors = new List<OktaFactor>();

            foreach (JToken t in value)
            {
                factors.Add(new OktaFactor(t));
            }

            value = JArray.Parse(this.AvailableFactorsRaw);

            foreach (JToken t in value)
            {
                OktaFactor f = new OktaFactor(t);
                if (!f.IsActive && factors.All(u => u.FactorTypeID != f.FactorTypeID))
                {
                    factors.Add(new OktaFactor(t));
                }
            }

            this.CollapseOktaVerifyFactors(factors);
            this.AddFactorsToTable(factors);
        }

        private void AddFactorsToTable(IEnumerable<OktaFactor> factors)
        {
            foreach (var f in factors.Where(t => !t.Skip))
            {
                this.AddRowToFactorTable(f);
            }
        }

        private void CollapseOktaVerifyFactors(List<OktaFactor> factors)
        {
            if (!AppConfigurationSection.CurrentConfig.CollapseOktaVerifyFactors)
            {
                return;
            }

            OktaFactor softToken = factors.FirstOrDefault(t =>
                string.Equals(t.Provider, "okta", StringComparison.OrdinalIgnoreCase)
                && string.Equals(t.FactorType, "token:software:totp", StringComparison.OrdinalIgnoreCase));

            OktaFactor push = factors.FirstOrDefault(t =>
                string.Equals(t.Provider, "okta", StringComparison.OrdinalIgnoreCase)
                && string.Equals(t.FactorType, "push", StringComparison.OrdinalIgnoreCase));

            if (push != null && softToken != null)
            {
                if (string.Equals(push.Status, softToken.Status, StringComparison.OrdinalIgnoreCase))
                {
                    push.IDsToReset.Add(softToken.ID);
                    softToken.Skip = true;
                }
            }
        }

        private void SetupFactorsTable()
        {
            this.factorTable.Rows.Clear();
            int rowCount = this.factorTable.Rows.Count;

            TableRow row = new TableRow { ID = $"row{rowCount}" };

            row.Cells.Add(new TableHeaderCell
            {
                Text = (string)this.GetLocalResourceObject("Factor"),
            });

            row.Cells.Add(new TableHeaderCell
            {
                Text = (string)this.GetLocalResourceObject("Status"),
            });

            row.Cells.Add(new TableHeaderCell
            {
                Text = (string)this.GetLocalResourceObject("LastUpdated"),
            });

            row.Cells.Add(new TableHeaderCell
            {
                Text = (string)this.GetLocalResourceObject("Reset"),
            });

            this.factorTable.Rows.Add(row);

            SD.Trace.WriteLine($"Row {rowCount} added");
        }

        private void AddRowToFactorTable(OktaFactor factor)
        {
            int rowCount = this.factorTable.Rows.Count;

            TableRow row = new TableRow { ID = $"row{rowCount}" };

            row.Cells.Add(new TableHeaderCell
            {
                Text = factor.DisplayName
            });

            row.Cells.Add(new TableCell
            {
                Text = factor.Status,
            });

            if (factor.LastUpdated.HasValue)
            {
                row.Cells.Add(new TableCell
                {
                    Text = factor.LastUpdated.Value.ToString("s") + @".000Z",
                    ID = $"timez{rowCount}",
                    CssClass = "timecell"
                });
            }
            else
            {
                row.Cells.Add(new TableCell());
            }

            if (this.HasWritePermission && factor.CanReset)
            {
                Button button = new Button();
                button.CssClass = "button";
                button.Text = (string)this.GetLocalResourceObject("ResetFactor");
                button.CommandName = "ResetFactor";
                button.CommandArgument = string.Join("~", factor.IDsToReset) + "$" + factor.FactorTypeID;
                button.Command += this.Button_Command;
                button.ID = $"button-reset-factor{rowCount}";

                button.UseSubmitBehavior = false;

                TableCell tc = new TableCell
                {
                    ID = $"cell{rowCount}",
                };

                tc.Controls.Add(button);
                row.Cells.Add(tc);
            }
            else
            {
                row.Cells.Add(new TableCell());
            }

            this.factorTable.Rows.Add(row);

            SD.Trace.WriteLine($"Row {rowCount} added");
        }

        private void Button_Command(object sender, CommandEventArgs e)
        {
            try
            {
                this.GetResource();

                if (!this.HasWritePermission)
                {
                    this.SetError((string)this.GetLocalResourceObject("AccessDenied"));
                    return;
                }

                var split = ((string)e.CommandArgument).Split('$');

                if (split.Length <= 1)
                {
                    throw new ArgumentException();
                }

                string factorType = split[split.Length - 1] ?? "Unknown";
                this.ResetFactors(split[0].Split('~'), factorType);
                this.ShowFactorResetSuccess();
                this.GetEnrolledFactors();
                this.PopulateFactorTable();
            }
            catch (Exception ex)
            {
                this.SetError($"The factor could not be reset. {ex.Message}");
                SD.Trace.WriteLine(ex.ToString());
            }
        }

        private void AddRowToUserTable(string header, string value)
        {
            int rowCount = this.userInfoTable.Rows.Count;

            TableRow row = new TableRow { ID = $"userInfoTablerow{rowCount}" };

            row.Cells.Add(new TableHeaderCell
            {
                Text = header,
                ID = $"th{rowCount}"
            });

            row.Cells.Add(new TableCell
            {
                Text = value,
                ID = $"cell{rowCount}"
            });

            this.userInfoTable.Rows.Add(row);

            SD.Trace.WriteLine($"Row {rowCount} added");
        }

        private void AddRowToUserTable(string value)
        {
            int rowCount = this.userInfoTable.Rows.Count;

            TableRow row = new TableRow { ID = $"userInfoTablerow{rowCount}" };

            row.Cells.Add(new TableCell
            {
                Text = value,
                ID = $"cell{rowCount}"
            });

            this.userInfoTable.Rows.Add(row);

            SD.Trace.WriteLine($"Row {rowCount} added");
        }

        private void ShowFactorResetSuccess()
        {
            this.resultRow.Visible = true;

            this.divPasswordSetMessage.Visible = true;
            this.lbPasswordSetMessage.Text = (string)this.GetLocalResourceObject("ResetFactorSuccess");
        }
    }
}