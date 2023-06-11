program Comp13;
var
   a, b, c: integer;

begin
   readLn ();
   readLn (a);
   while  a < 10 do
   begin
      writeln ("Value of a is :", a);
      a := a + 1;
      b := 0;
        while  b < 10 do
        begin
        writeln ("Value of b is :", b);
        b := b + 1;
		c := 0;
			while  c < 5 do
			begin
			writeln ("Value of c is :", c);
			c := c + 1;
			if (c > 2) then
			break 3;
			end;
        end;
   end;
end.