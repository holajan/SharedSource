using System;
using System.Collections.Generic;

namespace SilverlightDataValidation
{
    internal class PropertyValidator
    {
        #region member varible and default property initialization
        private Dictionary<string, List<string>> m_PropertyErrors = new Dictionary<string, List<string>>();
        #endregion

        #region action methods
        public bool Validate(string propertyName, Func<bool> ruleCheck, string message, bool removeOtherErrors = true)
        {
            if (m_PropertyErrors.ContainsKey(propertyName))
            {
                //Remove error message
                m_PropertyErrors[propertyName].Remove(message);
            }

            bool value = ruleCheck();
            if (value)
            {
                if (removeOtherErrors)
                {
                    m_PropertyErrors.Remove(propertyName);
                }

                if (!m_PropertyErrors.ContainsKey(propertyName))
                {
                    m_PropertyErrors.Add(propertyName, new List<string>());
                }

                //Add error message
                m_PropertyErrors[propertyName].Add(message);
            }

            if (m_PropertyErrors.ContainsKey(propertyName) && m_PropertyErrors[propertyName].Count == 0)
            {
                m_PropertyErrors.Remove(propertyName);
            }

            return value;
        }

        public string GetErrors(string propertyName)
        {
            if (m_PropertyErrors.ContainsKey(propertyName))
            {
                return string.Join(Environment.NewLine, m_PropertyErrors[propertyName]);
            }

            return null;
        }
        #endregion
    }
}
