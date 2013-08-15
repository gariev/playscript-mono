#!/usr/bin/perl


foreach my $arg(@ARGV) {
	my %classes = ();
	my @lines = ();

	open(FILE, $arg);
	while (<FILE>) {
		my $line = $_;
		# Add type defintions to variables
		$line =~ s/var +([a-zA-Z_0-9]+)([ =;]+)(?!:)+/var $1:*$2/;
		$line =~ s/const +([a-zA-Z_0-9]+)([ =;]+)(?!:)+/const $1:*$2/;

		# Track which class names we've seen
		if ($line =~ /class[ ]+([a-zA-Z_0-9]+)/) {
			$_ =~ /class[ ]+([a-zA-Z_0-9]+)/;
			$classes{$1} = 1;
		}

		# Add return types to functions, excluding the constructor
		if ($line =~ /function[ ]+([a-zA-Z_0-9]+)/) {
			$_ =~ /function[ ]+([a-zA-Z_0-9]+)/;
			if (!exists $classes{$1}) {
				$line =~ s/(function[ ]+[a-zA-Z_0-9]+)(\(.*\))([ ]*){(.*$)/$1$2:*$3\{$4/;
				$line =~ s/(function[ ]+[a-zA-Z_0-9]+)(\(.*\))([ ]*);(.*$)/$1$2:*$3\;$4/; # interfaces too
			}
		}

		push (@lines, $line);
	}
	close(FILE);

	open(FILE, ">$arg");
	print FILE @lines;
	close(FILE);
}

