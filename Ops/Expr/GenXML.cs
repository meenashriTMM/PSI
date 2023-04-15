using System.Xml.Linq;
namespace PSI;

// An basic XML code generator, implemented using the Visitor pattern
public class ExprXMLGen : Visitor<XElement> {
   public override XElement Visit (NLiteral literal)
      => new XElement ("Literal", new XAttribute ("Value", literal.Value.Text), new XAttribute ("Type", literal.Type));

   public override XElement Visit (NIdentifier identifier)
      => new XElement ("Ident", new XAttribute ("Name", identifier.Name.Text), new XAttribute ("Type", identifier.Type));

   public override XElement Visit (NUnary unary) {
      var elem = new XElement ("Unary", new XAttribute ("Op", unary.Op.Kind), new XAttribute ("Type", unary.Type));
      elem.Add (unary.Expr.Accept (this));
      return elem;
   }

   public override XElement Visit (NBinary binary) {
      var elem = new XElement ("Binary", new XAttribute ("Op", binary.Op.Kind), new XAttribute ("Type", binary.Type));
      elem.Add (binary.Left.Accept (this));
      elem.Add (binary.Right.Accept (this));
      return elem;
   }
}