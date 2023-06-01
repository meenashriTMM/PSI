program Comp6;
var
  i, j: integer;
  ch: char;
begin
  for i := 1 to 10 do begin
    Write (i, " ");
  end;
  WriteLn ();
  for i := 10 downto 1 do begin
    Write (i, " ");
  end;
  WriteLn ();
  for ch := 'a' to 'i' do begin
    Write (ch, " ");
  end;
  WriteLn ();
  for ch := 'z' downto 't' do begin
    Write (ch, " ");
  end;
  WriteLn ();
end.

