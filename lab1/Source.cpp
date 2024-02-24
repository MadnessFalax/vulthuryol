/**
* 
*		PJP - lab1		by: AUJ0009
* 
*/

#include <iostream>
#include <string>
#include <vector>

class Node {
protected:
	char code;
	bool err;

public:
	Node() {
		code = '\0';
		err = false;
	}

	virtual ~Node() {
		// std::cout << "called wrong destructor!" << std::endl;
	}

	char get_code() {
		return code;
	}

	virtual int evaluate() = 0;

	virtual bool made_error() {
		return err;
	}
};

class Num : public Node {
	int num;

public:
	Num(int n) {
		code = 'n';
		num = n;
		err = false;
	}

	Num(int n, bool err) {
		code = 'n';
		num = n;
		this->err = err;
	}

	~Num() override {
		// std::cout << "calling correct destructor!" << std::endl;
	}

	int evaluate() override {
		return num;
	}
};

class Operator : public Node {
protected:
	char op;

public:
	Operator(char op) {
		code = 'o';
		this->op = op;
		err = false;

		if (op != '+' && op != '-' && op != '*' && op != '/')
			err = true;
	}

	~Operator() override {
		// std::cout << "calling correct destructor!" << std::endl;

	}

	char get_op() {
		return op;
	}
	
	int evaluate(int lhs, int rhs) {
		switch (op) {
			case '+': {
				return add(lhs, rhs);
			}
			case '-': {
				return sub(lhs, rhs);
			}
			case '*': {
				return mul(lhs, rhs);
			}
			case '/': {
				if (rhs == 0)
					this->err = true;
				return div(lhs, rhs);
			}
			default: {
				// should not occur
				throw;
				break;
			}
		}
	}

	int evaluate() override {
		err = true;
		return 0;
	}

	int get_op_priority() {
		if (op == '/')
			return 0;
		if (op == '*')
			return 1;
		if (op == '-')
			return 2;
		if (op == '+')
			return 3;
		return 0;
	}

private:
	static int add(int lhs, int rhs) {
		return lhs + rhs;
	}

	static int sub(int lhs, int rhs) {
		return lhs - rhs;
	}

	static int mul(int lhs, int rhs) {
		return lhs * rhs;
	}

	static int div(int lhs, int rhs) {
		if (rhs == 0) {
			return 0;
		}
		return lhs / rhs;
	}
};

class Expression : public Node {
	std::vector<Node*> nodes;

public:
	Expression() {
		nodes = std::vector<Node*>();
		code = 'e';
		err = false;
	}

	Expression(std::vector<Node*> vec_in) {
		nodes = vec_in;
		code = 'e';
		err = false;
	}

	~Expression() override {
		// std::cout << "calling correct destructor!" << std::endl;
		for (auto node : nodes) {
			delete node;
			node = nullptr;
		}
		nodes.clear();
	}

	int evaluate() override {
		int tmp = 0;
		bool err = false;

		// parenthesis first
		for (int i = 0; i < nodes.size(); i++) {
			if (nodes[i]->get_code() == 'p') {
				tmp = nodes[i]->evaluate();
				err = nodes[i]->made_error();
				delete nodes[i];
				nodes[i] = nullptr;
				nodes[i] = new Num(tmp, err);
			}
		}
		
		tmp = 0;
		err = false;
		int max_priority = 4;
		int nodes_size = nodes.size();

		//now operators from lowest value priority to highest value priority
		for (int i = 0; i < max_priority; i++) {
			for (int j = 0; j < nodes_size; j++) {
				if (nodes[j]->get_code() == 'o') {
					Operator* cast_node = dynamic_cast<Operator*>(nodes[j]);

					if (cast_node->get_op_priority() == i) {
						if ((j - 1) < 0 || (j + 1) >= nodes.size()) {
							this->err = true;
							return 0;
						}
						if (nodes[j - 1]->get_code() != 'n' || nodes[j + 1]->get_code() != 'n') {
							this->err = true;
							return 0;
						}
						tmp = cast_node->evaluate(nodes[j - 1]->evaluate(), nodes[j + 1]->evaluate());
						err = cast_node->made_error() || nodes[j - 1]->made_error() || nodes[j + 1]->made_error();

						delete nodes[j + 1];
						nodes[j + 1] = nullptr;
						nodes.erase(nodes.begin() + j + 1);
						
						delete nodes[j];
						nodes[j] = nullptr;
						nodes.erase(nodes.begin() + j);
						
						delete nodes[j - 1];
						nodes[j - 1] = new Num(tmp, err);
						nodes_size -= 2;

					}
				}
			}
		}

		if (nodes.size() == 1) {
			this->err = nodes[0]->made_error();
			return nodes[0]->evaluate();
		}

		this->err = true;
		return -1;
	}

	/*void AddOperator() {

	}

	void AddNum() {

	}

	void AddParenthes() {

	}*/
};

class Parenthesis : public Node {
	Expression* subexpr;

public:
	Parenthesis(Expression* s) {
		code = 'p';
		subexpr = s;
		err = false;
	}

	~Parenthesis() override {
		// std::cout << "calling correct destructor!" << std::endl;
		delete subexpr;
		subexpr = nullptr;
	}

	int evaluate() override {
		int tmp = subexpr->evaluate();
		if (subexpr->made_error())
			err = true;
		return tmp;
	}
};

class Parser {
	bool eos;
	bool error;
	int index;
	std::string buf;
	char cur_char;
	int input_size;

public:
	Parser(std::string& buf) {
		eos = false;
		error = false;
		index = -1;
		this->buf = buf;
		cur_char = '\0';
		input_size = this->buf.length();
	}

	~Parser() {

	}

	bool made_error() {
		return this->error;
	}

	Expression* parse() {
		next_char();
		return parse_expression(false);
	}

private:
	void next_char() {
		if (index + 1 == input_size) {
			eos = true;
		}
		else {
			index++;
			cur_char = buf[index];
			if (cur_char == ' ' || cur_char == 9 /*tab key*/) {
				next_char();
			}
		}
	}

	Expression* parse_expression(bool parenth) {
		std::vector<Node*> collection = std::vector<Node*>();
		while (index < input_size && !eos) {
			if (cur_char >= '0' && cur_char <= '9') {
				collection.push_back(parse_number());
			}
			else if (cur_char == '(') {
				collection.push_back(parse_parenthesis());
			}
			else if (cur_char == ')') {
				if (parenth)
					break;
				else {
					this->error = true;
					break;
				}

			}
			else if (cur_char == '*') {
				collection.push_back(new Operator('*'));
				next_char();
			}
			else if (cur_char == '/') {
				collection.push_back(new Operator('/'));
				next_char();
			}
			else if (cur_char == '+') {
				collection.push_back(new Operator('+'));
				next_char();
			}
			else if (cur_char == '-') {
				collection.push_back(new Operator('-'));
				next_char();
			}
			// unknown character
			else {
				this->error = true;
				break;
			}
		}
		if (eos && parenth && cur_char != ')')
			this->error = true;
		return new Expression(collection);
	}

	Num* parse_number() {
		int tmp = 0;
		while (cur_char >= '0' && cur_char <= '9' && !eos) {
			tmp *= 10;
			tmp += cur_char - '0';
			next_char();
		}

		return new Num(tmp);
	}

	Parenthesis* parse_parenthesis() {
		Expression* ex_ptr = nullptr;
		next_char();
		ex_ptr = parse_expression(true);
		next_char();
		return new Parenthesis(ex_ptr);
	}


};

int main() {
	int input_count;
	std::string cur_input = std::string("");
	Expression* expr = nullptr;
	int tmp_res = 0;

	std::cin >> input_count;
	std::cin.ignore(1024, '\n');

	for (int i = 0; i < input_count; i++) {
		std::getline(std::cin, cur_input, '\n');

		Parser a(cur_input);
		
		expr = a.parse();

		if (a.made_error())
			std::cout << "ERROR" << std::endl;
		else {
			tmp_res = expr->evaluate();
			if (expr->made_error()) {
				std::cout << "ERROR" << std::endl;
			}
			else {
				std::cout << tmp_res << std::endl;
			}
		}

		delete expr;
		cur_input.clear();
	}


	return 0;
}