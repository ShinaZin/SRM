;Из каждой пары чисел выводит максимальное число
.org 2		;reserve(0..1)

$_start:
	in		
	str [0]	;mem[0] = in(a)
	in
	str [1]	;mem[1] = in(b)
	sub [0]	;rx = b-a
	
	ifn		;if rx < 0
		jmp $_ifneg
	ldr [1]	;else rx=load(b)
	jmp $_output
	
$_ifneg:	
	ldr [0]	;then rx=load(a)

$_output:
	out		;out(rx)
jmp $_start

