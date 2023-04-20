// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// PSIPrint.cs ~ Prints a PSI syntax tree in Pascal format
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;

public class PSIPrint : Visitor<StringBuilder> {
   #region Main, Declarations --------------------------------------------------
   public override StringBuilder Visit (NProgram p) {
      Write ($"program {p.Name}; ");
      Visit (p.Block);
      return Write (".");
   }

   public override StringBuilder Visit (NBlock b) 
      => Visit (b.Decls, b.Body);

   public override StringBuilder Visit (NDeclarations d) {
      if (d.Vars.Length > 0) {
         NWrite ("var"); N++;
         foreach (var g in d.Vars.GroupBy (a => a.Type))
            NWrite ($"{g.Select (a => a.Name).ToCSV ()} : {g.Key};");
         N--;
      }
      d.FuncProcs.ForEach (a => Visit (a));
      return S;
   }

   public override StringBuilder Visit (NVarDecl d)
      => Write ($"{d.Name}: {d.Type}");

   public override StringBuilder Visit (NFuncProcDecl d) {
      NWrite (""); NWrite ("");
      var procedure = d.Type == NType.None;
      NWrite (procedure ? "procedure" : "function");
      Write ($" {d.Name.Text} (");
      for (int i = 0; i < d.Params.Length; i++) {
         if (i > 0) Write ("; "); d.Params[i].Accept (this);
      }
      Write (")");
      if (!procedure) Write ($" : {d.Type}");
      Write (";");
      Visit (d.Block);
      return Write (";");
   }
   #endregion

   #region Statement -----------------------------------------------------------
   public override StringBuilder Visit (NCompoundStmt b) {
      NWrite ("begin"); N++;  Visit (b.Stmts); N--; return NWrite ("end"); 
   }

   public override StringBuilder Visit (NAssignStmt a) {
      NWrite ($"{a.Name} := "); a.Expr.Accept (this); return Write (";");
   }

   public override StringBuilder Visit (NWriteStmt w) {
      NWrite (w.NewLine ? "WriteLn (" : "Write (");
      for (int i = 0; i < w.Exprs.Length; i++) {
         if (i > 0) Write (", ");
         w.Exprs[i].Accept (this);
      }
      return Write (");");
   }

   public override StringBuilder Visit (NReadStmt r)
      => NWrite ($"read ({r.Args.Select (a => a.Text).ToCSV ()});");

   public override StringBuilder Visit (NWhileStmt w) {
      NWrite ("while "); Visit (w.Expr); Write (" do");
      N++; Visit (w.Stmt); N--;
      return Semi (w.Stmt);
   }

   public override StringBuilder Visit (NCallStmt c) {
      NWrite ($"{c.Name} (");
      for (int i = 0; i < c.Args.Length; i++) {
         if (i > 0) Write (", "); c.Args[i].Accept (this);
      }
      return Write (");");
   }

   public override StringBuilder Visit (NIfStmt i) {
      NWrite ("if "); Visit (i.Expr); Write (" then");
      var stmt = i.Stmts[0];
      N++; Visit (stmt); Semi (stmt); N--;
      if (i.Stmts.Length > 1) {
         stmt = i.Stmts[1];
         NWrite ("else"); N++; Visit (stmt); Semi (stmt); N--;
      }
      return S;
   }

   public override StringBuilder Visit (NRepeatUntilStmt w) {
      NWrite ("repeat "); N++; Visit (w.Stmts); N--; NWrite ("until ");
      return Visit (w.Expr);
   }

   public override StringBuilder Visit (NForStmt f) {
      NWrite ($"for {f.Variable.Text} := "); Visit (f.Exprs[0]);
      Write (f.isTo ? " to " : " downto ");
      Visit (f.Exprs[1]); Write (" do");
      N++; Visit (f.Stmt); Semi (f.Stmt); N--;
      return S;
   }
   #endregion

   #region Expression ----------------------------------------------------------
   public override StringBuilder Visit (NLiteral t)
      => Write (t.Value.ToString ());

   public override StringBuilder Visit (NIdentifier d)
      => Write (d.Name.Text);

   public override StringBuilder Visit (NUnary u) {
      Write (u.Op.Text); return u.Expr.Accept (this);
   }

   public override StringBuilder Visit (NBinary b) {
      Write ("("); b.Left.Accept (this); Write ($" {b.Op.Text} ");
      b.Right.Accept (this); return Write (")");
   }

   public override StringBuilder Visit (NFnCall f) {
      Write ($"{f.Name} (");
      for (int i = 0; i < f.Params.Length; i++) {
         if (i > 0) Write (", "); f.Params[i].Accept (this);
      }
      return Write (")");
   }
   #endregion

   StringBuilder Semi (NStmt stmt) { if (stmt is NCompoundStmt) Write (";"); return S; }

   StringBuilder Visit (params Node[] nodes) {
      nodes.ForEach (a => a.Accept (this));
      return S;
   }

   // Writes in a new line
   StringBuilder NWrite (string txt) 
      => Write ($"\n{new string (' ', N * 3)}{txt}");
   int N;   // Indent level

   // Continue writing on the same line
   StringBuilder Write (string txt) {
      Console.Write (txt);
      S.Append (txt);
      return S;
   }

   readonly StringBuilder S = new ();
}