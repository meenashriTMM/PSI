﻿// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// TypeAnalyze.cs ~ Type checking, type coercion
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;
using static NType;
using static Token.E;

public class TypeAnalyze : Visitor<NType> {
   public TypeAnalyze () {
      mSymbols = SymTable.Root;
   }
   SymTable mSymbols;

   #region Declarations ------------------------------------
   public override NType Visit (NProgram p) 
      => Visit (p.Block);
   
   public override NType Visit (NBlock b) {
      mSymbols = new SymTable { Parent = mSymbols };
      Visit (b.VarDecls);
      Visit (b.Declarations); Visit (b.Body);
      mSymbols = mSymbols.Parent;
      return Void;
   }

   public override NType Visit (NDeclarations d) {
      Visit (d.Constants); Visit (d.Vars); return Visit (d.Funcs);
   }

   public override NType Visit (NVarDecl d) {
      Exists (d.Name);
      mSymbols.Vars.Add (d);
      return d.Type;
   }

   public override NType Visit (NConstDecl c) {
      Exists (c.Name);
      mSymbols.Constants.Add (c);
      return Visit (c.Value);
   }

   public override NType Visit (NFnDecl f) {
      Exists (f.Name);
      mSymbols.Funcs.Add (f);
      f.Body?.Accept (this);
      return f.Return;
   }
   #endregion

   #region Statements --------------------------------------
   public override NType Visit (NCompoundStmt b)
      => Visit (b.Stmts);

   public override NType Visit (NAssignStmt a) {
      NType type = mSymbols.Find (a.Name.Text) switch {
         NVarDecl v => v.Type,
         NFnDecl f => f.Return,
         _ => throw new ParseException (a.Name, "Unknown variable")
      };
      a.Expr.Accept (this);
      a.Expr = AddTypeCast (a.Name, a.Expr, type);
      return type;
   }
   
   NExpr AddTypeCast (Token token, NExpr source, NType target) {
      if (source.Type == target) return source;
      bool valid = (source.Type, target) switch {
         (Int, Real) or (Char, Int) or (Char, String) => true,
         _ => false
      };
      if (!valid) throw new ParseException (token, "Invalid type");
      return new NTypeCast (source) { Type = target };
   }

   public override NType Visit (NWriteStmt w)
      => Visit (w.Exprs);

   public override NType Visit (NIfStmt f) {
      f.Condition.Accept (this);
      f.IfPart.Accept (this); f.ElsePart?.Accept (this);
      return Void;
   }

   public override NType Visit (NForStmt f) {
      f.Start.Accept (this); f.End.Accept (this); f.Body.Accept (this);
      return Void;
   }

   public override NType Visit (NReadStmt r) {
      var var = r.Vars.FirstOrDefault (a => mSymbols.Find (a.Text) is not NVarDecl);
      if (var != null) throw new ParseException (var, $"Name \"{var.Text}\" does not exist in the current context");
      return Void;
   }

   public override NType Visit (NWhileStmt w) {
      w.Condition.Accept (this); w.Body.Accept (this);
      return Void; 
   }

   public override NType Visit (NRepeatStmt r) {
      Visit (r.Stmts); r.Condition.Accept (this);
      return Void;
   }

   public override NType Visit (NCallStmt c) {
      Call (c.Name, c.Params);
      return Void;
   }
   #endregion

   #region Expression --------------------------------------
   public override NType Visit (NLiteral t) {
      t.Type = t.Value.Kind switch {
         L_INTEGER => Int, L_REAL => Real, L_BOOLEAN => Bool, L_STRING => String,
         L_CHAR => Char, _ => Error,
      };
      return t.Type;
   }

   public override NType Visit (NUnary u) 
      => u.Type = u.Expr.Accept (this);

   public override NType Visit (NBinary bin) {
      NType a = bin.Left.Accept (this), b = bin.Right.Accept (this);
      bin.Type = (bin.Op.Kind, a, b) switch {
         (ADD or SUB or MUL or DIV, Int or Real, Int or Real) when a == b => a,
         (ADD or SUB or MUL or DIV, Int or Real, Int or Real) => Real,
         (MOD, Int, Int) => Int,
         (ADD, String, _) => String, 
         (ADD, _, String) => String,
         (LT or LEQ or GT or GEQ, Int or Real, Int or Real) => Bool,
         (LT or LEQ or GT or GEQ, Int or Real or String or Char, Int or Real or String or Char) when a == b => Bool,
         (EQ or NEQ, _, _) when a == b => Bool,
         (EQ or NEQ, Int or Real, Int or Real) => Bool,
         (AND or OR, Int or Bool, Int or Bool) when a == b => a,
         _ => Error,
      };
      if (bin.Type == Error)
         throw new ParseException (bin.Op, "Invalid operands");
      var (acast, bcast) = (bin.Op.Kind, a, b) switch {
         (_, Int, Real) => (Real, Void),
         (_, Real, Int) => (Void, Real), 
         (_, String, not String) => (Void, String),
         (_, not String, String) => (String, Void),
         _ => (Void, Void)
      };
      if (acast != Void) bin.Left = new NTypeCast (bin.Left) { Type = acast };
      if (bcast != Void) bin.Right = new NTypeCast (bin.Right) { Type = bcast };
      return bin.Type;
   }

   public override NType Visit (NIdentifier d) {
      return mSymbols.Find (d.Name.Text) switch {
         NVarDecl v => d.Type = v.Type,
         NConstDecl c => d.Type = c.Value.Type,
         _ => throw new ParseException (d.Name, "Unknown variable")
      };
   }

   public override NType Visit (NFnCall f) => f.Type = Call (f.Name, f.Params);

   public override NType Visit (NTypeCast c) {
      c.Expr.Accept (this); return c.Type;
   }
   #endregion

   #region Helper functions --------------------------------
   NType Call (Token name, NExpr[] parameters) {
      if (mSymbols.Find (name.Text) is not NFnDecl fnDecl)
         throw new ParseException (name, $"Function \"{name.Text}\" not found");
      var len = parameters.Length;
      if (len != fnDecl.Params.Length)
         throw new ParseException (name, $"No overload of \"{name.Text}\" method takes {len} arguments");
      for (int i = 0; i < len; i++) {
         parameters[i].Accept (this);
         parameters[i] = AddTypeCast (name, parameters[i], fnDecl.Params[i].Type);
      }
      return fnDecl.Return;
   }

   void Exists (Token tok) {
      if (mSymbols.Find (tok.Text, true) != null)
         throw new ParseException (tok, $"Name \"{tok.Text}\" already exists");
   }

   NType Visit (IEnumerable<Node> nodes) {
      foreach (var node in nodes) node.Accept (this);
      return NType.Void;
   }
   #endregion
}
