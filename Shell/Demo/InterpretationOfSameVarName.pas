program CTest;
var
  i : integer;

procedure Test (a: real; b: string; c: integer);
begin
  write (a,b,c);
end;

procedure Test1 (i: string);
begin
  i := 10;
  write (i);
end;

begin
  Test (1, 'a', 'b');
  Test1 ("Hello");
end.