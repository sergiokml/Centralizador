//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Centralizador.Models.Properties {
    using System;
    
    
    /// <summary>
    ///   Clase de recurso fuertemente tipado, para buscar cadenas traducidas, etc.
    /// </summary>
    // StronglyTypedResourceBuilder generó automáticamente esta clase
    // a través de una herramienta como ResGen o Visual Studio.
    // Para agregar o quitar un miembro, edite el archivo .ResX y, a continuación, vuelva a ejecutar ResGen
    // con la opción /str o recompile su proyecto de VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Devuelve la instancia de ResourceManager almacenada en caché utilizada por esta clase.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Centralizador.Models.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Reemplaza la propiedad CurrentUICulture del subproceso actual para todas las
        ///   búsquedas de recursos mediante esta clase de recurso fuertemente tipado.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Busca una cadena traducida similar a &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;xsl:stylesheet xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot; xmlns:sii=&quot;http://www.sii.cl/SiiDte&quot; xmlns:str=&quot;http://exslt.org/strings&quot; version=&quot;1.0&quot;&gt;
        ///  &lt;xsl:output method=&quot;xml&quot; indent=&quot;yes&quot; /&gt;
        ///  &lt;!--&lt;xsl:template match=&quot;@* | node()&quot;&gt; : Atributo + comentarios + Texto nodo y elemento. LO MISMO: attribute::* | child::node()
        ///      &lt;xsl:template match=&quot;@* | *&quot;&gt;      : Atributo y elemento.
        ///      &lt;xsl:template match=&quot;/&quot;&gt;           : Nodo raíz.--&gt;
        ///
        ///  &lt;!--. Nodo actu [resto de la cadena truncado]&quot;;.
        /// </summary>
        internal static string EncoderXmlToHtml {
            get {
                return ResourceManager.GetString("EncoderXmlToHtml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Busca una cadena traducida similar a IF NOT EXISTS (
        ///		SELECT *
        ///		FROM sys.objects
        ///		WHERE name = &apos;IW_GSAEN_REF_DTE_CEN&apos;
        ///			AND type = &apos;TR&apos;
        ///		)
        ///	EXEC dbo.sp_executesql @statement = 
        ///		N&apos;
        ///
        ///CREATE TRIGGER [softland].[IW_GSAEN_REF_DTE_CEN] ON [softland].[iw_gsaen_refdte]
        ///AFTER INSERT
        ///AS
        ///DECLARE @TipoDte VARCHAR(1);
        ///
        ///SELECT @TipoDte = (
        ///		SELECT i.Tipo
        ///		FROM inserted i
        ///		);--F / N
        ///
        ///IF @TipoDte = &apos;&apos;F&apos;&apos;
        ///BEGIN
        ///	SET NOCOUNT ON;
        ///	DECLARE @FolioRef VARCHAR(40);-- DE04457A19C47
        ///	DECLARE @RazonRef VARCHAR(40);--SEN_[]
        ///	DECLARE @Fc [resto de la cadena truncado]&quot;;.
        /// </summary>
        internal static string sql_insert_Trigger {
            get {
                return ResourceManager.GetString("sql_insert_Trigger", resourceCulture);
            }
        }
    }
}
